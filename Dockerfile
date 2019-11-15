FROM stephenlautier/netcore-docker-spa:3.0.0
ARG source
ARG appName
ARG appVersion
ARG gitCommit

ENV APP_NAME $appName
ENV APP_VERSION $appVersion
ENV GIT_COMMIT $gitCommit
ENV DOCKER true
ENV NODE_OPTIONS --max-old-space-size=3072

WORKDIR /app
EXPOSE 80
COPY ${source:-obj/Docker/publish} .

#HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
        #CMD curl -f http://localhost/health/ || exit 1

ENTRYPOINT ["dotnet", "Sketch7.DotnetNgx.dll"]