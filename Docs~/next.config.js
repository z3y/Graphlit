const withNextra = require('nextra')({
    theme: 'nextra-theme-docs',
    themeConfig: './theme.config.jsx',
})

const nextConfig = {
    output: 'export',
    images: {
        unoptimized: true,
        },
        assetPrefix: './',
        basePath: '',
}

// https://www.viget.com/articles/host-build-and-deploy-next-js-projects-on-github-pages/
const isGithubActions = process.env.GITHUB_ACTIONS || false
if (isGithubActions) {
    const repo = process.env.GITHUB_REPOSITORY.replace(/.*?\//, '')

    nextConfig.assetPrefix = `/${repo}`
    nextConfig.basePath = `/${repo}`
}

module.exports = withNextra(nextConfig)

// If you have other Next.js configurations, you can pass them as the parameter:
// module.exports = withNextra({ /* other next.js config */ })