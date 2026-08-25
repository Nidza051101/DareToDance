
import axios from 'axios';

const api = axios.create({
    baseURL : 'http://localhost:5015',
});

api.interceptors.request.use((config) => {
    const token = localStorage.getItem('accessToken');

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
            const refreshToken = localStorage.getItem('refreshToken');
            const response = await api.post('/auth/refresh', { refreshToken });
            const data = response.data;
            localStorage.setItem('accessToken', data.accessToken);
            original.headers.Authorization = `Bearer ${data.accessToken}`;
            return api(original);
        } catch (err) {
            localStorage.clear();
            window.location.href = '/index.html';
            return Promise.reject(err);
        }
    }

    return Promise.reject(error);
  }
);

export default api;
