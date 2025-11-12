import axios from 'axios';

const portalClient = axios.create({
    baseURL: import.meta.env.VITE_PORTAL_BASEURL + '/api',
    withCredentials: true,
});

portalClient.interceptors.request.use((config) => {
    // if () {
    //     config.headers.Authorization = `Bearer ${'token'}`;
    // }
    return config;
});

export { portalClient };