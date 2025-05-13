module PortfolioLib.Types

type DailyPrice = {
    Date : System.DateTime
    Open : float
    High : float
    Low : float
    Close : float
    Volume : float
}

type ReturnPoint = {
    Date : System.DateTime
    Return : float
}

type PortfolioResult = {
    Ticker : string
    AnnualizedReturn : float
    AnnualizedVolatility : float
    SharpeRatio : float
}
