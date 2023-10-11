// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function getSwamData() {
    const httpUrl = "http://localhost:5254";
    const apiCommClient = "/api/client"; //Perform get
    const apiCommJobP = "/api/jobpost"; //Perform get

    const dataHolder = {
        clientId = 0,
        ipAddr = "",
        portNum = 0;
        completedJobs = 0;
    }

    function sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    //Creating a dynamic list of clients and jobs
    function generateListItem(dataObject) {

    }

    //Performing get data
    function getData() {
        fetch(httpUrl + apiCommClient)
            .then(response => {
                if (!response.ok) {
                    throw new Error(" Get Data Failed!: clientData");
                }
                return response.json();
            })
            .then(data => {
                fetch(httpUrl + apiCommJobP)
                    .then(response2 => {
                        if (!response2.ok) {
                            throw new Error(" Get Data Failed!: JobData");
                        }
                        return response2.json();
                    })
                    .then(data2 => {
                        const parsedData = JSON.parse(data);
                        const tempData = dataHolder;
                        for (let i = 0; i < parsedData.length; i++) {
                            tempData.clientId = parsedData[i].clientId;
                            tempData.ipAddr = parsedData[i].ipAddr;
                            tempData.portNum = parsedData[i].portNum;
                        }
                    })
            })
            .catch(error => {
                console.error("Error occurred: ", error);
            });
    }

    //Auto-Updating interface every 1 minute
    async function autoGUIUpdate() {
        while (true) {
            //await sleep(60000);
            await sleep(10000) // 10 seconds
            getData();
        }
    }
}
