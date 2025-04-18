import axios from "axios";
import React, { createContext, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import { UserProfile } from "../Models/User";
import { LoginApi, RegisterApi } from "../Services/Auth";

type UserCotextType = {
    user: UserProfile | null;
    token: string | null;
    RegisterUser: (email: string, username: string, password: string)=>void;
    LoginUser: (username: string, password: string)=>void;
    LogOut: ()=>void;
    IsLogedIn: ()=>boolean;
}

type props = { children: React.ReactNode };

const UserContext = createContext<UserCotextType>({} as UserCotextType);

export const UserProvider = ( {children}: props)=>{

    const navigate = useNavigate();

    const [token, setToken] = useState<string|null>(null);
    const [user, setUser] = useState<UserProfile|null>(null);
    const [isReady, setisReady]  = useState<Boolean>(false);

    useEffect(() => {
        const user = localStorage.getItem("user");
        const token = localStorage.getItem("token");

        if(user && token){
            setUser(JSON.parse(user));
            setToken(token);
            axios.defaults.headers.common["Authorization"] = "Bearer " + token;
        };

        setisReady(true);
    },[]);

    const RegisterUser = async ( email: string, username: string, password: string ) => {

        await RegisterApi(email,username,password).then((res) =>{
            if(res){
                localStorage.setItem("token", res?.data.token);
                const userobj = {
                    username: res?.data.username , 
                    email: res?.data.email
                };
                
                localStorage.setItem("user", JSON.stringify(userobj))
                setToken(res?.data.token);
                setUser(userobj!)

                toast.success("Login Succes!");
                navigate("/search");
            }
        }).catch((e) => toast.warning("server error ocurred"))
    }


    const LoginUser = async ( username: string, password: string ) => {

        await LoginApi( username, password ).then((res) =>{

            if(res){

                localStorage.setItem("token", res?.data.token);
                const userobj = {
                    username: res?.data.username , 
                    email: res?.data.email
                };
                
                localStorage.setItem("user", JSON.stringify(userobj))
                setToken(res?.data.token);
                setUser(userobj!)

                toast.success("Login Succes!");
                navigate("/search");
            }
        }).catch((e) => toast.warning("server error ocurred"))
    }

    const IsLogedIn = () =>{
        return !!user
    }

    const LogOut = () => {
        localStorage.clear();
        setToken(null);
        setUser(null);
        navigate("/")
    }

    return (
        <UserContext.Provider 
            value={{LoginUser, RegisterUser, user, token, IsLogedIn, LogOut}}
        >
            {isReady ? children: null}
        </UserContext.Provider>
    );
};

export const UserAuth = () => React.useContext(UserContext)