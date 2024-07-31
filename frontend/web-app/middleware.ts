export { default } from "next-auth/middleware"

export const config = {
    // Define the routes that should be protected by the middleware
    matcher: [
        '/session' // Apply middleware to the /session route
    ], 
     // Define custom pages for authentication
    pages: {
        signIn: '/api/auth/signin' // Custom sign-in page route
    }
}