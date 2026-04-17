/** @type {import('next').NextConfig} */
const nextConfig = {
  output: "export", // enables static HTML export
  images: {
    unoptimized: true, // required for static export
  },
};

module.exports = nextConfig;