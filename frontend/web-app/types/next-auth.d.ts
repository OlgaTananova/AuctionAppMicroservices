// Importing the DefaultSession type from next-auth
import { DefaultSession } from 'next-auth';

// Extending the 'next-auth' module to add custom properties to the Session and Profile interfaces
declare module 'next-auth' {
    // Extending the Session interface to include a username property within the user object
    interface Session {
        user: {
            username: string  // Adding username property to the user object
        } & DefaultSession['user']  // Merging with the default user properties from DefaultSession
    }

    // Extending the Profile interface to include a username property
    interface Profile {
        username: string  // Adding username property to the Profile interface
    }
}

// Extending the 'next-auth/jwt' module to add custom properties to the JWT interface
declare module 'next-auth/jwt' {
    // Extending the JWT interface to include username and access_token properties
    interface JWT {
        username: string  // Adding username property to the JWT interface
        access_token?: string  // Adding optional access_token property to the JWT interface
    }
}
