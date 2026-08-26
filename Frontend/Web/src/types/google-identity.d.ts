// Minimal shape for the part of Google Identity Services we actually use -
// not the full SDK surface, just initialize/renderButton and the response.
interface GoogleCredentialResponse {
  credential: string;
}

declare global {
  interface Window {
    google?: {
      accounts: {
        id: {
          initialize(config: {
            client_id: string;
            callback: (response: GoogleCredentialResponse) => void;
          }): void;
          renderButton(
            parent: HTMLElement,
            options: { theme?: string; size?: string; text?: string; width?: number },
          ): void;
        };
      };
    };
  }
}

export {};
