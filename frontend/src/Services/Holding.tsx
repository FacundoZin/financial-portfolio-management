import axios from "axios";
import { HandleError } from "../Helpers/HandlerError";
import { holdingGet, HoldingPost } from "../Models/Holding";

const api = "https://localhost:52203/api/holding/";

export const HoldingAddAPI = async (symbol: string) => {
    try{
        const Result = await axios.post<HoldingPost>(api + `?symbol=${symbol}`);
        return Result; 
    }catch(error){
        HandleError(error);
    }
};

export const HoldingDeleteAPI = async (symbol: string) => {
    try{
        const Result = await axios.delete<HoldingPost>(api + `${symbol}`);
        return Result;
    }catch(error){
        HandleError(error);
    }
};

export const HoldingGetAPI = async () => {
    try{
        const Result = await axios.get<holdingGet>(api)
        return Result;
    }catch(error){
        HandleError(error);
    }
};

