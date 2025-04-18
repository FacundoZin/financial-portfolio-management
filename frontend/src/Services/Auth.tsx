import axios from "axios";
import { HandleError } from "../Helpers/HandlerError";
import { UserProfileToken } from "../Models/User";

const api = "https://localhost:52203/api"

export const LoginApi = async (username: string, password: string) => {
    try{
        const data = await axios.post<UserProfileToken>(api + "account/login", {
            username: username,
            password: password
        });
        return data
    }catch(error){
        HandleError(error);
    }
}

export const RegisterApi = async (email: string, username: string, password: string) => {
    try{
        const data = await axios.post<UserProfileToken>(api + "account/register", {
            email: email,
            username: username,
            password: password
        });
        return data
    }catch(error){
        HandleError(error);
    }
}