// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

const httpUrl = "http://localhost:5254";
const apiCommClient = "/api/data/client"; //Perform get
const apiCommJobP = "/api/data/jobpost"; //Perform get

const dataHolder = {
    clientId: 0,
    ipAddr: "",
    portNum: 0,
    completedJobs: 0
}

function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

//Creating a dynamic list of clients and jobs
function generateListItem(dataObject) {
    var list = document.getElementById("clientList");
    list.innerHTML += `<li>` +
        `<p>Client ID: ${dataObject.clientId} </p>` +
        `<p>Client IP: ${dataObject.ipAddr}</p>` +
        `<p>Client Port: ${dataObject.portNum}</p>` +
        `<p>Completed Jobs: ${dataObject.completedJobs}</p>` +
        `</li>`;
}

function clearList() {
    var list = document.getElementById("clientList");
    list.innerHTML = "";
}

const additionalData = {
    method: 'GET',
    headers: {
        'Content-Type': 'application/json'
    }
}

//Performing get data
function getData() {
    fetch(apiCommClient, additionalData)
        .then(response => {
            if (!response.ok) {
                clearList();
                throw new Error(" Get Data Failed!: clientData (" + response.status + ")");
            }
            return response.json();
        })
        .then(data => {
            fetch(apiCommJobP, additionalData)
                .then(response2 => {
                    if (!response2.ok) {
                        throw new Error(`Get Data Failed!: JobData (${response2.status})`);
                    }
                    return response2.json();
                })
                .then(data2 => {
                    console.log(data);
                    console.log(data2);
                    const parsedDataClient = JSON.parse(JSON.stringify(data));
                    const parsedDataJob = JSON.parse(JSON.stringify(data2));
                    console.log(parsedDataClient);
                    console.log(parsedDataJob);
                    const tempData = dataHolder;
                    let count;
                    clearList();
                    for (let i = 0; i < parsedDataClient.length; i++) {
                        count = 0;
                        tempData.clientId = parsedDataClient[i].clientId;
                        tempData.ipAddr = parsedDataClient[i].ipAddr;
                        tempData.portNum = parsedDataClient[i].portNum;

                        //Counting the number of jobs the client has done for other clients
                        for (let j = 0; j < parsedDataJob.length; j++) {
                            if (tempData.clientId == parsedDataJob[j].ToClient) {
                                count++;
                            }
                        }
                        tempData.completedJobs = count;
                        generateListItem(tempData);
                    }
                })
                .catch(error2 => {
                    console.error("Error occured: ", error2);
                });
        })
        .catch(error => {
            console.error("Error occurred: ", error);
        });
}

//Auto-Updating interface every 1 minute
async function autoGUIUpdate() {
    console.log("GUI update begin");
    while (true) {
        //await sleep(60000);
        await sleep(10000) // 10 seconds
        getData();
    }
}

//Execute
autoGUIUpdate();

