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
      duration: '1m'
    }
  }
}

export default function() {
  const response = http.post('http://localhost:5042/api/players/endturnall');
  check(response, {
    'status is 200': (r) => r.status === 200
  })
}

