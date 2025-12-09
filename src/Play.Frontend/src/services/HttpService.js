import axios from 'axios';
import UserService from './UserService';

const HttpMethods = {
  GET: 'get',
  POST: 'post',
  PUT: 'put',
  DELETE: 'delete',
}

const _axios = axios.create({ baseURL: 'https://localhost:8000' });

const configure = () => {
  _axios.interceptors.request.use(async (config) => {
    const cb = () => {
      config.headers.Authorization = `Bearer ${UserService.getToken()}`;
      return Promise.resolve(config);
    };
    return UserService.updateToken(cb);
  });
};

const getAxiosClient = () => _axios;

const HttpService = {
  HttpMethods,
  configure,
  getAxiosClient
};

export default HttpService;
