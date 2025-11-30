export const BUILD_VERSION = import.meta.env.VITE_APP_VERSION || 'dev'
export const BUILD_DATE = import.meta.env.VITE_APP_BUILD_DATE || new Date().toISOString()
