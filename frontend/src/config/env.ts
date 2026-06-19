const REQUIRED_ENV_KEYS = [
  'VITE_API_BASE_URL',
  'VITE_API_VERSION',
  'VITE_API_TIMEOUT_MS',
] as const

type RequiredEnvKey = (typeof REQUIRED_ENV_KEYS)[number]

const ENV_SETUP_HINT = 'Copy frontend/.env.example to frontend/.env and configure the required variables.'

function isUnset(value: string | undefined): boolean {
  return value === undefined || String(value).trim() === ''
}

function getRequiredEnv(name: RequiredEnvKey): string {
  const value = (import.meta.env as Record<string, string | undefined>)[name]
  if (isUnset(value)) {
    throw new Error(`Missing required environment variable ${name}. ${ENV_SETUP_HINT}`)
  }

  return String(value).trim()
}

function getPositiveNumberFromEnv(name: 'VITE_API_TIMEOUT_MS'): number {
  const raw = getRequiredEnv(name)
  const parsed = Number(raw)
  if (!Number.isFinite(parsed) || parsed <= 0) {
    throw new Error(`Environment variable ${name} must be a positive number. Received: ${raw}`)
  }

  return parsed
}

export const env = {
  apiBaseUrl: getRequiredEnv('VITE_API_BASE_URL').replace(/\/$/, ''),
  apiVersion: getRequiredEnv('VITE_API_VERSION'),
  apiTimeoutMs: getPositiveNumberFromEnv('VITE_API_TIMEOUT_MS'),
}
