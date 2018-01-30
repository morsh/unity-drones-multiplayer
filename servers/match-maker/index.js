const Api = require('kubernetes-client');
const yaml = require('yamljs');

let core;
try {
  core = new Api.Core(Api.config.getInCluster());
} catch (e) {
  core = new Api.Core(Api.config.fromKubeconfig());
}

const express = require('express');
const app = express();
const port = 8081;

// core.ns("default").po.get(pod => {
//   console.log(pod);
// });

core.ns("default").pods.get((error, pods) => {

  console.log(JSON.stringify(pods));
});

app.get('/', (request, response) => {

  response.send('Hello from Express!')
});

app.get('/create', (request, response) => {

  // const manifest0 = {
  //   kind: 'Deployment',
  //   apiVersion: 'extensions/v1beta1'
    
  // };

  yaml.load('file.yml', (manifest) => {
      nativeObject = manifest;

      core.group(manifest).ns.kind(manifest).post({ body: manifest }, (err, result) => {

        if (error) {
          console.error(err);
          response.send('There was an error..');
          return;
        }
    
        console.log(result);
        response.send('Hello from Express!');
      });
  });
});

app.listen(port, (err) => {
  if (err) {
    return console.log('something bad happened', err)
  }

  console.log(`server is listening on ${port}`)
});