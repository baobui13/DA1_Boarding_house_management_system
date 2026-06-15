#!/bin/bash
TOKEN=$(curl -s -X POST http://localhost:5046/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@qlt.com", "password":"Password123!"}' | jq -r '.token')

echo "Token: $TOKEN"

curl -s "http://localhost:5046/api/Property/GetModerationProperties?status=Pending&pageNumber=1&pageSize=8" \
  -H "Authorization: Bearer $TOKEN" | jq
