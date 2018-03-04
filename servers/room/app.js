const dgram = require('dgram');
const udpPort = process.env.UDPPORT || 5701;

const MAX_CONNECTIONS = 100;

let port = 5701;

let hostId;
let webHostId;

let reliableChannel;
let unreliableChannel;

let isStarted = false;
let error;

let clients = new Array();

let lastMovementUpdate;
let movementUpdateRate = 0.05;

const server = dgram.createSocket('udp4');
server.bind(udpPort);

server.on('listening', () => {
  console.log('Server started at ', udpPort);
});

server.on('message', (msg, info) => {
  let message = msg.toString(); // need to convert to string 
  console.log(message);
  // since message is received as buffer 
  // receive the message and do task
});

server.on('error', () => {
  let errorA = 1;
  // handle error
});