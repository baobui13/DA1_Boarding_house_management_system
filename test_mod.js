fetch("http://localhost:5046/api/Property/GetModerationProperties?status=Approved&pageNumber=1&pageSize=1000", {
    headers: { "Authorization": "Bearer fake" } // need valid token or temp disable Authorize
}).then(r=>r.json()).then(console.log);
