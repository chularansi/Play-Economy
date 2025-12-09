import { createContext } from "react";
// import { initialState, authReducer } from "./authReducer";

const AuthContext = createContext();

export default AuthContext;

// export const AuthProvider = ({ children }) => {
//   const [state, dispatch] = useReducer(authReducer, initialState);

//   return (
//     <AuthContext.Provider value={{ state, dispatch }}>
//       {children}
//     </AuthContext.Provider>
//   );
// };