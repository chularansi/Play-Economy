import { Container, Nav, Navbar } from 'react-bootstrap';
import { Link } from 'react-router-dom';
import { ApplicationPaths } from './Constants';
import Keycloak from './api-authorization/Keycloak';
import React, { useEffect, useContext } from 'react';
import AuthContext from '../state-management/contexts/AuthContext';

export function NavMenu() {
  const { state, dispatch } = useContext(AuthContext);

  useEffect(() => {
    Keycloak.init({
      onLoad: 'check-sso',
      scope:
        'openid profile email catalog-api-fullaccess inventory-api-fullaccess userinfo-api-fullaccess trading-api-fullaccess',
      silentCheckSsoRedirectUri:
        window.location.origin + '/silent-check-sso.html',
      pkceMethod: 'S256',
      redirectUri: 'http://localhost:3000/',
    }).then((auth) => {
      if (auth) {
        Keycloak.loadUserInfo()
          .then((user) => {
            console.log('User profile loaded:', user);
            dispatch({
              type: 'LOGIN_SUCCESS',
              payload: { user: user, token: Keycloak.token },
            });
          })
          .catch((error) => {
            console.error('Failed to load user profile:', error);
          });
      } else {
        dispatch({
          type: 'LOGIN_FAILURE',
          payload: { error: 'User not authenticated' },
        });
      }
    });
  }, [dispatch]);

  const logout = () => {
    Keycloak.logout();
    dispatch({ type: 'LOGOUT' });
  };

  // const hasRole = (roles) => roles.some((role) => Keycloak.hasRealmRole(role));

  return (
    <header>
      <Navbar bg="light" expand="lg">
        <Container>
          <Navbar.Brand as={Link} to="/">
            Play Economy
          </Navbar.Brand>
          <Navbar.Toggle aria-controls="basic-navbar-nav" />
          <Navbar.Collapse id="basic-navbar-nav">
            {state.isAuthenticated ? (
              <>
                {/* {hasRole(['Admin']) && hasRole(['Player']) ? ( */}
                {state.user.role.includes('Admin') ? (
                  <Nav className="mr-auto">
                    <Nav.Link as={Link} to="/">
                      Home
                    </Nav.Link>
                    <Nav.Link as={Link} to={ApplicationPaths.StorePath}>
                      Store
                    </Nav.Link>
                    <Nav.Link as={Link} to={ApplicationPaths.InventoryPath}>
                      My Inventory
                    </Nav.Link>
                    <Nav.Link as={Link} to={ApplicationPaths.CatalogPath}>
                      Catalog
                    </Nav.Link>
                    <Nav.Link as={Link} to={ApplicationPaths.UsersPath}>
                      Users
                    </Nav.Link>
                  </Nav>
                ) : (
                  <Nav className="mr-auto">
                    <Nav.Link as={Link} to="/">
                      Home
                    </Nav.Link>
                    <Nav.Link as={Link} to={ApplicationPaths.StorePath}>
                      Store
                    </Nav.Link>
                    <Nav.Link as={Link} to={ApplicationPaths.InventoryPath}>
                      My Inventory
                    </Nav.Link>
                  </Nav>
                )}
                <Nav>
                  <Nav.Link as={Link} to="#">
                    Welcome {state.user.given_name}
                  </Nav.Link>
                  <Nav.Link as={Link} to="#" onClick={logout}>
                    Logout
                  </Nav.Link>
                </Nav>
              </>
            ) : (
              <>
                <Nav className="mr-auto">
                  <Nav.Link as={Link} to="/">
                    Home
                  </Nav.Link>
                </Nav>
                <Nav>
                  <Nav.Link as={Link} to="#" onClick={() => Keycloak.login()}>
                    Login
                  </Nav.Link>
                </Nav>
              </>
            )}
          </Navbar.Collapse>
        </Container>
      </Navbar>
    </header>
  );
}
