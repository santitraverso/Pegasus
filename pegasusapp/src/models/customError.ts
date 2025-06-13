export interface CustomError extends Error {
  status?: number
  errorCode?: string
  serverMessage?: string
}