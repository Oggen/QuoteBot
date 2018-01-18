const AWS = require("aws-sdk");
AWS.config.update({ region: "us-east-1" });
const dynamodb = new AWS.DynamoDB();
const tableName = "Quotes";

exports.handler = (event, context, callback) => {
    if (event.currentIntent.name === "DisputeLastQuote" && event.invocationSource === "DialogCodeHook") {
        dynamodb.scan({ TableName: tableName }, (err, data) => {
            if (err) { console.log(err); }
            else {
                data.Items.forEach(item => {
                    if (item.AddedOn) {
                        item.AddedOn.N = +item.AddedOn.N;
                    }
                    else {
                        item.AddedOn = { N: 0 };
                    }
                });

                let quotes = data.Items.filter(q => q.Quotee.S === "@" + event.sessionAttributes.mentionName)
                quotes.sort((a, b) => { return b.AddedOn.N - a.AddedOn.N });

                if (quotes.length === 0 || quotes[0].AddedOn.N === 0) {
                    callback(null, {
                        dialogAction: {
                            type: "Close",
                            fulfillmentState: "Failed",
                            message: {
                                contentType: "PlainText",
                                content: "Unable to determine the most recent quote."
                            }
                        }
                    });
                }
                else {
                    let recentQuote = quotes[0];
                    callback(null, {
                        sessionAttributes: {
                            quote: recentQuote.Quote.S,
                            quotee: recentQuote.Quotee.S
                        },
                        dialogAction: {
                            type: "Delegate",
                            slots: {}
                        }
                    });
                }
            }
        });
    }
};
