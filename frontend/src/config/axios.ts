import axios from 'axios'
import { env } from './env'
import { parseApiBody } from '../utils/apiResponse'

const sharedConfig = {
  baseURL: env.apiBaseUrl,
  timeout: env.apiTimeoutMs,
  headers: {
    Accept: 'application/json',
  },
  maxRedirects: 0,
}

export const api = axios.create(sharedConfig)

api.interceptors.response.use(
  (response) => {
    response.data = parseApiBody(response.data)
    return response
  },
  (error) => Promise.reject(error),
)
