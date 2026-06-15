const http = require('http');

http.get('http://localhost:5046/api/Property/GetPropertiesByFilter?landlordId=test', (resp) => {
  let data = '';
  resp.on('data', (chunk) => { data += chunk; });
  resp.on('end', () => {
    console.log("Response:", JSON.parse(data).items.length);
  });
}).on("error", (err) => {
  console.log("Error: " + err.message);
});
