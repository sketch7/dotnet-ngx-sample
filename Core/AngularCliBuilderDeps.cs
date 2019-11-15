// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.NodeServices.Util;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// todo: move these into lib if not AngularCliBuilder PR
// these are purely copy/paste

// ref: https://github.com/aspnet/AspNetCore/blob/master/src/Middleware/SpaServices.Extensions/src/Npm/NodeScriptRunner.cs
namespace Microsoft.AspNetCore.NodeServices.Npm
{
	/// <summary>
	/// Executes the <c>script</c> entries defined in a <c>package.json</c> file,
	/// capturing any output written to stdio.
	/// </summary>
	internal class NodeScriptRunner
	{
		public EventedStreamReader StdOut { get; }
		public EventedStreamReader StdErr { get; }

		private static Regex AnsiColorRegex = new Regex("\x001b\\[[0-9;]*m", RegexOptions.None, TimeSpan.FromSeconds(1));

		public NodeScriptRunner(string workingDirectory, string scriptName, string arguments, IDictionary<string, string> envVars, string pkgManagerCommand)
		{
			if (string.IsNullOrEmpty(workingDirectory))
			{
				throw new ArgumentException("Cannot be null or empty.", nameof(workingDirectory));
			}

			if (string.IsNullOrEmpty(scriptName))
			{
				throw new ArgumentException("Cannot be null or empty.", nameof(scriptName));
			}

			if (string.IsNullOrEmpty(pkgManagerCommand))
			{
				throw new ArgumentException("Cannot be null or empty.", nameof(pkgManagerCommand));
			}

			var exeToRun = pkgManagerCommand;
			var completeArguments = $"run {scriptName} -- {arguments ?? string.Empty}";
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				// On Windows, the node executable is a .cmd file, so it can't be executed
				// directly (except with UseShellExecute=true, but that's no good, because
				// it prevents capturing stdio). So we need to invoke it via "cmd /c".
				exeToRun = "cmd";
				completeArguments = $"/c {pkgManagerCommand} {completeArguments}";
			}

			var processStartInfo = new ProcessStartInfo(exeToRun)
			{
				Arguments = completeArguments,
				UseShellExecute = false,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				WorkingDirectory = workingDirectory
			};

			if (envVars != null)
			{
				foreach (var keyValuePair in envVars)
				{
					processStartInfo.Environment[keyValuePair.Key] = keyValuePair.Value;
				}
			}

			var process = LaunchNodeProcess(processStartInfo, pkgManagerCommand);
			StdOut = new EventedStreamReader(process.StandardOutput);
			StdErr = new EventedStreamReader(process.StandardError);
		}

		public void AttachToLogger(ILogger logger)
		{
			// When the node task emits complete lines, pass them through to the real logger
			StdOut.OnReceivedLine += line =>
			{
				if (!string.IsNullOrWhiteSpace(line))
				{
					// Node tasks commonly emit ANSI colors, but it wouldn't make sense to forward
					// those to loggers (because a logger isn't necessarily any kind of terminal)
					logger.LogInformation(StripAnsiColors(line));
				}
			};

			StdErr.OnReceivedLine += line =>
			{
				if (!string.IsNullOrWhiteSpace(line))
				{
					logger.LogError(StripAnsiColors(line));
				}
			};

			// But when it emits incomplete lines, assume this is progress information and
			// hence just pass it through to StdOut regardless of logger config.
			StdErr.OnReceivedChunk += chunk =>
			{
				var containsNewline = Array.IndexOf(
					chunk.Array, '\n', chunk.Offset, chunk.Count) >= 0;
				if (!containsNewline)
				{
					Console.Write(chunk.Array, chunk.Offset, chunk.Count);
				}
			};
		}

		private static string StripAnsiColors(string line)
			=> AnsiColorRegex.Replace(line, string.Empty);

		private static Process LaunchNodeProcess(ProcessStartInfo startInfo, string commandName)
		{
			try
			{
				var process = Process.Start(startInfo);

				// See equivalent comment in OutOfProcessNodeInstance.cs for why
				process.EnableRaisingEvents = true;

				return process;
			}
			catch (Exception ex)
			{
				var message = $"Failed to start '{commandName}'. To resolve this:.\n\n"
							+ $"[1] Ensure that '{commandName}' is installed and can be found in one of the PATH directories.\n"
							+ $"    Current PATH enviroment variable is: { Environment.GetEnvironmentVariable("PATH") }\n"
							+ "    Make sure the executable is in one of those directories, or update your PATH.\n\n"
							+ "[2] See the InnerException for further details of the cause.";
				throw new InvalidOperationException(message, ex);
			}
		}
	}
}


namespace Microsoft.AspNetCore.NodeServices.Util
{
	/// <summary>
	/// Wraps a <see cref="StreamReader"/> to expose an evented API, issuing notifications
	/// when the stream emits partial lines, completed lines, or finally closes.
	/// </summary>
	internal class EventedStreamReader
	{
		public delegate void OnReceivedChunkHandler(ArraySegment<char> chunk);
		public delegate void OnReceivedLineHandler(string line);
		public delegate void OnStreamClosedHandler();

		public event OnReceivedChunkHandler OnReceivedChunk;
		public event OnReceivedLineHandler OnReceivedLine;
		public event OnStreamClosedHandler OnStreamClosed;

		private readonly StreamReader _streamReader;
		private readonly StringBuilder _linesBuffer;

		public EventedStreamReader(StreamReader streamReader)
		{
			_streamReader = streamReader ?? throw new ArgumentNullException(nameof(streamReader));
			_linesBuffer = new StringBuilder();
			Task.Factory.StartNew(Run);
		}

		public Task<Match> WaitForMatch(Regex regex)
		{
			var tcs = new TaskCompletionSource<Match>();
			var completionLock = new object();

			OnReceivedLineHandler onReceivedLineHandler = null;
			OnStreamClosedHandler onStreamClosedHandler = null;

			void ResolveIfStillPending(Action applyResolution)
			{
				lock (completionLock)
				{
					if (!tcs.Task.IsCompleted)
					{
						OnReceivedLine -= onReceivedLineHandler;
						OnStreamClosed -= onStreamClosedHandler;
						applyResolution();
					}
				}
			}

			onReceivedLineHandler = line =>
			{
				var match = regex.Match(line);
				if (match.Success)
				{
					ResolveIfStillPending(() => tcs.SetResult(match));
				}
			};

			onStreamClosedHandler = () =>
			{
				ResolveIfStillPending(() => tcs.SetException(new EndOfStreamException()));
			};

			OnReceivedLine += onReceivedLineHandler;
			OnStreamClosed += onStreamClosedHandler;

			return tcs.Task;
		}

		private async Task Run()
		{
			var buf = new char[8 * 1024];
			while (true)
			{
				var chunkLength = await _streamReader.ReadAsync(buf, 0, buf.Length);
				if (chunkLength == 0)
				{
					if (_linesBuffer.Length > 0)
					{
						OnCompleteLine(_linesBuffer.ToString());
						_linesBuffer.Clear();
					}

					OnClosed();
					break;
				}

				OnChunk(new ArraySegment<char>(buf, 0, chunkLength));

				int lineBreakPos = -1;
				int startPos = 0;

				// get all the newlines
				while ((lineBreakPos = Array.IndexOf(buf, '\n', startPos, chunkLength - startPos)) >= 0 && startPos < chunkLength)
				{
					var length = (lineBreakPos + 1) - startPos;
					_linesBuffer.Append(buf, startPos, length);
					OnCompleteLine(_linesBuffer.ToString());
					_linesBuffer.Clear();
					startPos = lineBreakPos + 1;
				}

				// get the rest
				if (lineBreakPos < 0 && startPos < chunkLength)
				{
					_linesBuffer.Append(buf, startPos, chunkLength);
				}
			}
		}

		private void OnChunk(ArraySegment<char> chunk)
		{
			var dlg = OnReceivedChunk;
			dlg?.Invoke(chunk);
		}

		private void OnCompleteLine(string line)
		{
			var dlg = OnReceivedLine;
			dlg?.Invoke(line);
		}

		private void OnClosed()
		{
			var dlg = OnStreamClosed;
			dlg?.Invoke();
		}
	}
}

namespace Microsoft.AspNetCore.NodeServices.Util
{
	/// <summary>
	/// Captures the completed-line notifications from a <see cref="EventedStreamReader"/>,
	/// combining the data into a single <see cref="string"/>.
	/// </summary>
	internal class EventedStreamStringReader : IDisposable
	{
		private EventedStreamReader _eventedStreamReader;
		private bool _isDisposed;
		private StringBuilder _stringBuilder = new StringBuilder();

		public EventedStreamStringReader(EventedStreamReader eventedStreamReader)
		{
			_eventedStreamReader = eventedStreamReader
								   ?? throw new ArgumentNullException(nameof(eventedStreamReader));
			_eventedStreamReader.OnReceivedLine += OnReceivedLine;
		}

		public string ReadAsString() => _stringBuilder.ToString();

		private void OnReceivedLine(string line) => _stringBuilder.AppendLine(line);

		public void Dispose()
		{
			if (!_isDisposed)
			{
				_eventedStreamReader.OnReceivedLine -= OnReceivedLine;
				_isDisposed = true;
			}
		}
	}
}