/* eslint-disable global-require, import/no-extraneous-dependencies */
module.exports = (env) => {
	const isDevBuild = !(env); // todo check for env production mode

	const postcssConfig = {
		plugins: [require('autoprefixer')]
	};
	if (!isDevBuild) {
		postcssConfig.plugins.push(
			require('cssnano')({
				preset: ['default', {
					discardComments: {
						removeAll: true,
					},
				}]
			})
		);
	}

	return postcssConfig;
}