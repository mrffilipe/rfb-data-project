export const brandAssets = {
  logoLight: '/brand/kyvo-logo-light.png',
  logoDark: '/brand/kyvo-logo-dark.png',
  icon: '/favicon.svg',
} as const

export function brandLogoSrc(mode: 'light' | 'dark'): string {
  return mode === 'dark' ? brandAssets.logoDark : brandAssets.logoLight
}
