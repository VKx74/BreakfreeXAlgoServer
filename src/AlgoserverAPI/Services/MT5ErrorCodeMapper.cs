using System.Collections.Generic;

namespace Algoserver.API.Services
{
    public static class MT5ErrorCodeMapper
    {
        public static Dictionary<int, string> Errors = new Dictionary<int, string> {
            // HTTP web request errors
            {100, "Continue"},
            {101, "Switching Protocols"},
            {102, "Processing"},
            {200, "OK"},
            {201, "Created"},
            {202, "Accepted"},
            {203, "Non-Authoritative Information"},
            {204, "No Content"},
            {205, "Reset Content"},
            {206, "Partial Content"},
            {207, "Multi-Status"},
            {208, "Already Reported"},
            {226, "IM Used"},
            {300, "Multiple Choices"},
            {301, "Moved Permanently"},
            {302, "Found"},
            {303, "See Other"},
            {304, "Not Modified"},
            {305, "Use Proxy"},
            {306, "Reserved"},
            {307, "Temporary Redirect"},
            {308, "Permanent Redirect"},
            {400, "Bad Request"},
            {401, "Unauthorized"},
            {402, "Payment Required"},
            {403, "Forbidden"},
            {404, "Not Found"},
            {405, "Method Not Allowed"},
            {406, "Not Acceptable"},
            {407, "Proxy Authentication Required"},
            {408, "Request Timeout"},
            {409, "Conflict"},
            {410, "Gone"},
            {411, "Length Required"},
            {412, "Precondition Failed"},
            {413, "Request Entity Too Large"},
            {414, "Request-URI Too Long"},
            {415, "Unsupported Media Type"},
            {416, "Requested Range Not Satisfiable"},
            {417, "Expectation Failed"},
            {422, "Unprocessable Entity"},
            {423, "Locked"},
            {424, "Failed Dependency"},
            {425, "Unassigned"},
            {426, "Upgrade Required"},
            {427, "Unassigned"},
            {428, "Precondition Required"},
            {429, "Too Many Requests"},
            {430, "Unassigned"},
            {431, "Request Header Fields Too Large"},
            {500, "Internal Server Error"},
            {501, "Not Implemented"},
            {502, "Bad Gateway"},
            {503, "Service Unavailable"},
            {504, "Gateway Timeout"},
            {505, "HTTP Version Not Supported"},
            {506, "Variant Also Negotiates (Experimental)"},
            {507, "Insufficient Storage"},
            {508, "Loop Detected"},
            {509, "Unassigned"},
            {510, "Not Extended"},
            {511, "Network Authentication Required"},

            // MT5 place order errors
            {1001, "MetaTrader WebRequest internal error"},
            {10004, "Requote"},
            {10006, "Request rejected"},
            {10007, "Request canceled by trader"},
            {10008, "Order placed"},
            {10009, "Request completed"},
            {10010, "Only part of the request was completed"},
            {10011, "Request processing error"},
            {10012, "Request canceled by timeout"},
            {10013, "Invalid request"},
            {10014, "Invalid volume in the request"},
            {10015, "Invalid price in the request"},
            {10016, "Invalid stops in the request"},
            {10017, "Trade is disabled"},
            {10018, "Market is closed"},
            {10019, "There is not enough money to complete the request"},
            {10020, "Prices changed"},
            {10021, "There are no quotes to process the request"},
            {10022, "Invalid order expiration date in the request"},
            {10023, "Order state changed"},
            {10024, "Too frequent requests"},
            {10025, "No changes in request"},
            {10026, "Autotrading disabled by server"},
            {10027, "Autotrading disabled by client terminal"},
            {10028, "Request locked for processing"},
            {10029, "Order or position frozen"},
            {10030, "Invalid order filling type"},
            {10031, "No connection with the trade server"},
            {10032, "Operation is allowed only for live accounts"},
            {10033, "The number of pending orders has reached the limit"},
            {10034, "The volume of orders and positions for the symbol has reached the limit"},
            {10035, "Incorrect or prohibited order type"},
            {10036, "Position with the specified POSITION_IDENTIFIER has already been closed"},
            {10038, "A close volume exceeds the current position volume"},
            {10039, "A close order already exists for a specified position. This may happen when working in the hedging system: when attempting to close a position with an opposite one, while close orders for the position already exist when attempting to fully or partially close a position if the total volume of the already present close orders and the newly placed one exceeds the current position volume"},
            {10040, "The number of open positions simultaneously present on an account can be limited by the server settings. After a limit is reached, the server returns the TRADE_RETCODE_LIMIT_POSITIONS error when attempting to place an order. The limitation operates differently depending on the position accounting type: Netting — number of open positions is considered. When a limit is reached, the platform does not let placing new orders whose execution may increase the number of open positions. In fact, the platform allows placing orders only for the symbols that already have open positions. The current pending orders are not considered since their execution may lead to changes in the current positions but it cannot increase their number. Hedging — pending orders are considered together with open positions, since a pending order activation always leads to opening a new position. When a limit is reached, the platform does not allow placing both new market orders for opening positions and pending orders."},
            {10041, "The pending order activation request is rejected, the order is canceled"},
            {10042, "The request is rejected, because the \"Only long positions are allowed\" rule is set for the symbol (POSITION_TYPE_BUY)"},
            {10043, "The request is rejected, because the \"Only short positions are allowed\" rule is set for the symbol (POSITION_TYPE_SELL)"},
            {10044, "The request is rejected, because the \"Only position closing is allowed\" rule is set for the symbol"},
            {10045, "The request is rejected, because \"Position closing is allowed only by FIFO rule\" flag is set for the trading account (ACCOUNT_FIFO_CLOSE=true)"},
            {10046, "The request is rejected, because the \"Opposite positions on a single symbol are disabled\" rule is set for the trading account. For example, if the account has a Buy position, then a user cannot open a Sell position or place a pending sell order. The rule is only applied to accounts with hedging accounting system (ACCOUNT_MARGIN_MODE=ACCOUNT_MARGIN_MODE_RETAIL_HEDGING)."},
        };

        public static bool SkipMessage(string str)
        {
            if (str.Contains("Web") && str.Contains("1001"))
            {
                return true;
            }
            return false;
        }

        public static string ReplaceErrorCodeWithMessage(string str)
        {
            var stringParts = str.Split(" ");
            for (int i = 0; i < stringParts.Length; i++)
            {
                string s = stringParts[i];
                if (int.TryParse(s, out var code))
                {
                    if (Errors.TryGetValue(code, out var message))
                    {
                        stringParts[i] = $"{message} {code}";
                    }
                }
            }
            return string.Join(" ", stringParts);
        }
    }
}
