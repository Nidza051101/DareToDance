// accessToken u localStorage; refreshToken se NIGDE ne čuva na frontend
// strani — živi samo u HttpOnly kolačiću koji backend postavlja i JS ga
// nikad ne dodiruje. V. artifact "Token Storage Security".
const STORAGE_KEY = 'accessToken';

export const getAccessToken = () => localStorage.getItem(STORAGE_KEY);

export const setAccessToken = (token: string) => {
    localStorage.setItem(STORAGE_KEY, token);
};

export const clearAccessToken = () => {
    localStorage.removeItem(STORAGE_KEY);
};
