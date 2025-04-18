
import './index.css';
import NavBar from './Components/NavBar/NavBar';
import { Outlet } from 'react-router-dom';
import { ToastContainer } from "react-toastify";
import "react-toastify/dist/react/ReactToastify.css"
import { UserProvider } from './Context/UserAuth';


function App() {

  return (
    <>
    <UserProvider>
    <NavBar />
    <Outlet />
    <ToastContainer />
    </UserProvider> 
    </>
  );

}
export default App
