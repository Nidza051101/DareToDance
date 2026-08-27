import axios from 'axios';
import { getAccessToken, setAccessToken } from './authToken';

const api = axios.create({
    baseURL: 'http://localhost:5015',
    withCredentials: true,
});

api.interceptors.request.use((config) => {
    const token = getAccessToken();

    if(token)
    {
        config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
});

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config;

    if (error.response?.status === 401 && !original._retry) {
        original._retry = true;

        try {
            const response = await api.post('/auth/refresh');
            const data = response.data;
            setAccessToken(data.accessToken);
            original.headers.Authorization = `Bearer ${data.accessToken}`;
            return api(original);
        } catch (err) {
            window.location.href = '/index.html';
            return Promise.reject(err);
        }
    }

    return Promise.reject(error);
  }
);

export default api;