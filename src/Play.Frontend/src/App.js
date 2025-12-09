import React, { useReducer } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { Catalog } from './components/Catalog';
import { Inventory } from './components/Inventory';
import { Users } from './components/Users';
import { Store } from './components/Store';
// import AuthorizeRoute from './components/api-authorization/AuthorizeRoute';
// import ApiAuthorizationRoutes from './components/api-authorization/ApiAuthorizationRoutes';
// import { AuthorizationPaths } from './components/api-authorization/ApiAuthorizationConstants';
import { ApplicationPaths } from './components/Constants';
import { initialState, authReducer } from "./state-management/reducers/authReducer";
import './App.css'
import AuthContext from './state-management/contexts/AuthContext';

export default function App() {
  // static displayName = App.name;
  const [state, dispatch] = useReducer(authReducer, initialState);

    return (
      <AuthContext.Provider value={{ state, dispatch }}>
        <Layout>
          <Route exact path='/' component={Home} />
          {/* <AuthorizeRoute path={ApplicationPaths.CatalogPath} component={Catalog} />
          <AuthorizeRoute path={ApplicationPaths.InventoryPath} component={Inventory} />
          <AuthorizeRoute path={ApplicationPaths.UsersPath} component={Users} />
          <Route path={AuthorizationPaths.ApiAuthorizationPrefix} component={ApiAuthorizationRoutes} /> */}
          <Route path={ApplicationPaths.CatalogPath} component={Catalog} />
          <Route path={ApplicationPaths.InventoryPath} component={Inventory} />
          <Route path={ApplicationPaths.UsersPath} component={Users} />
          <Route path={ApplicationPaths.StorePath} component={Store} />
          {/* <Route path={AuthorizationPaths.ApiAuthorizationPrefix} component={UserMenu} /> */}
        </Layout>
      </AuthContext.Provider>
    );
  
}
