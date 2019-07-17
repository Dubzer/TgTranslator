// Thanks devSchacht for writing base of this code https://medium.com/devschacht/node-hero-chapter-4-c2ebcd12565c
const franc = require('franc-min')
const http = require('http')
const port = 41873

const requestHandler = (request, response) => {
    var requestString = Buffer.from(request.headers.text, 'base64').toString('ascii')
    console.log(`Got a request: ${requestString}`)

    var language = franc(requestString);
    console.log(`- Result: ${language}`);
    response.end(language);
}

const server = http.createServer(requestHandler)
server.listen(port, (error) => {
    if (error) {
        return console.log('Something bad happened', error)
    }
    console.log(`Server is listening on ${port}`)
})