import Keycloak from 'keycloak-js';

const _kc = new Keycloak({
  url: 'http://localhost:8080/',
  realm: 'play-auth-microservice',
  clientId: 'react-frontend-microservice',
});

/**
 * Initializes Keycloak instance and calls the provided callback function if successfully authenticated.
 *
 * @param onAuthenticatedCallback
 */
const initKeycloak = () => {
  console.log('Keycloak initilized');
  // const initKeycloak = (onAuthenticatedCallback) => {
  _kc
    .init({
      onLoad: 'check-sso',
      silentCheckSsoRedirectUri:
        window.location.origin + '/silent-check-sso.html',
      pkceMethod: 'S256',
      redirectUri: 'http://localhost:3000/',
    })
    .then((authenticated) => {
      if (!authenticated) {
        console.log('user is not authenticated..!');
        // window.location.reload();
      }
      // onAuthenticatedCallback();
    })
    .catch(console.error);
};

const doLogin = _kc.login;

const doLogout = _kc.logout;

const getToken = () => _kc.token;

const getTokenParsed = () => _kc.tokenParsed;

const isLoggedIn = () => !!_kc.token;

const updateToken = (successCallback) =>
  _kc.updateToken(5).then(successCallback).catch(doLogin);

const getUsername = () => _kc.tokenParsed?.preferred_username;

const getFirstname = () => _kc.tokenParsed?.given_name;

const hasRole = (roles) => roles.some((role) => _kc.hasRealmRole(role));

const UserService = {
  initKeycloak,
  doLogin,
  doLogout,
  isLoggedIn,
  getToken,
  getTokenParsed,
  updateToken,
  getUsername,
  getFirstname,
  hasRole,
};

export default UserService;
