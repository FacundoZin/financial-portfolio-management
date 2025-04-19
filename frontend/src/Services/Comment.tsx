import axios from "axios";
import { HandleError } from "../Helpers/HandlerError";
import { CommentPost } from "../Models/Comment";

const api = "https://localhost:52203/api/comment/"


export const PostCommentAPI = async (title: string, content: string, symbol: string) =>{
    try{
        const result = await axios.post<CommentPost>(api + `${symbol}`,{
            title: title,
            content: content,
        });
        return result

    }catch(error){
        HandleError(error);
    }
};

export const GetCommentAPI = async (symbol: string) =>{
    try{
        const result = await axios.get<CommentPost[]>(api + `?Symbol=${symbol}`);
        return result

    }catch(error){
        HandleError(error);
    }
};

