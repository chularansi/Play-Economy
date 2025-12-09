import Keycloak from 'keycloak-js';

const keycloak = new Keycloak({
  url: 'http://localhost:8080/',
  realm: 'play-auth-microservice',
  clientId: 'react-frontend-microservice',
});

export default keycloak;
