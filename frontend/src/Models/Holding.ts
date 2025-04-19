export type holdingGet = {
    id: number;
    symbol: string;
    companyName: string;
    purchase: number;
    lastDiv: number;
    industry: string;
    marketCap: number;
    comments: any;
  };
  
  export type HoldingPost = {
    symbol: string;
  };