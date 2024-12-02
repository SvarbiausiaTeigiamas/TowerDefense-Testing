// k6 run buy-item.js --insecure-skip-tls-verify

import http from 'k6/http'
import {
  check,
} from 'k6'

export const options = {
  scenarios: {
    fixed_load: {
      executor: 'constant-vus',
      vus: 1000,
      duration: '5m'
    }
  }
}

export function setup() {
  const payload = JSON.stringify({
    itemId: "Rockets",
    playerName: "playerone"
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
    }
  };

  return { params, payload }
}

export default function(data) {
  const response = http.post('http://localhost:5042/api/shop', data.payload, data.params);
  check(response, {
    'status is 200': (r) => r.status === 200
  })
}

