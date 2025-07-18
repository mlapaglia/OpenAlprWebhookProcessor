const PROXY_CONFIG = [
  {
    context: [
      "/api/**",
    ],
    target: "https://localhost:5001/",
    secure: false,
    ws: true,
    changeOrigin: true,
  }
]

module.exports = PROXY_CONFIG;
