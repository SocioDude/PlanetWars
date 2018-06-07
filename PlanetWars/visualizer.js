class Drawer {
    constructor() {
        this.canvas = document.getElementById("main-canvas");
        this.context = this.canvas.getContext("2d");
        this.xOffset = 100;
        this.yOffset = 100;
        this.xMultiplier = 35;
        this.yMultiplier = 35;
        this.minRadius = 15;
        this.playerOneColor = "#00BFFF";
        this.playerTwoColor = "#DC143C";
        this.neutralColor = "#D3D3D3";
    }

    clear() {
        this.context.clearRect(0, 0, this.canvas.width, this.canvas.height);
    }

    translateX(x) {
        return x * this.xMultiplier + this.xOffset;
    }

    translateY(y) {
        return y * this.yMultiplier + this.yOffset;
    }

    getColor(owner) {
        if (owner === 0) {
            return this.neutralColor;
        } else if (owner === 1) {
            return this.playerOneColor;
        } else {
            return this.playerTwoColor;
        }
    }

    drawPlanet(planet) {
        // Draw the circle
        var path = new Path2D();
        path.arc(
            this.translateX(planet.x),
            this.translateY(planet.y),
            planet.growth + this.minRadius,
            0,
            2 * Math.PI);
        this.context.fillStyle = this.getColor(planet.owner);
        this.context.fill(path);

        // Write the number of ships.
        this.context.textAlign = 'center';
        this.context.textBaseline = 'middle';
        this.context.fillStyle = "#000000";
        this.context.font = "15px Arial";
        this.context.fillText(
            planet.ships,
            this.translateX(planet.x),
            this.translateY(planet.y));
    }

    drawFleet(fleet, source, destination) {
        // Figure out where the planets are.
        var sourceX = this.translateX(source.x);
        var sourceY = this.translateY(source.y);
        var destinationX = this.translateX(destination.x);
        var destinationY = this.translateY(destination.y);

        // Draw the line the fleet is going on.
        this.context.beginPath();
        this.context.moveTo(sourceX, sourceY);
        this.context.lineTo(destinationX, destinationY);
        this.context.strokeStyle = this.getColor(fleet.owner);
        this.context.stroke();
        this.context.closePath();

        // Draw the fleet position just as the number of ships.
        var distanceRatio = 1 - fleet.remainingTurns / fleet.totalTurns;
        var progressX =
            (1 - distanceRatio) * sourceX + distanceRatio * destinationX;
        var progressY =
            (1 - distanceRatio) * sourceY + distanceRatio * destinationY;
        console.log(progressX);
        console.log(progressY);
        this.context.textAlign = 'center';
        this.context.textBaseline = 'middle';
        this.context.fillStyle = this.getColor(fleet.owner);
        this.context.font = "12px Arial";
        this.context.fillText(fleet.ships, progressX, progressY);
    }
}

var drawer = new Drawer();
var turns = [];

function newGameClick() {
    drawer.clear();
    var gameData = document.getElementById("data").value;
    var splitData = gameData.split("\n");
    var currentTurnData = [];
    turns = [];
    for (var i = 0; i < splitData.length; i++) {
        // TODO: skim out blank lines.
        if (splitData[i] === "go") {
            turns.push(new Turn(currentTurnData));
            currentTurnData = [];
        } else {
            currentTurnData.push(splitData[i]);
        }
    }

    renderGame(0);
}

function renderGame(currentTurn) {
    if (currentTurn >= turns.length) {
        return;
    }

    drawer.clear();
    turns[currentTurn].draw();
    setTimeout(function () { renderGame(currentTurn + 1); }, 100);
}

class Turn {
    // A bunch of lines of replay data, no line that says "go"
    constructor(lines) {
        this.planets = [];
        this.fleets = [];
        for (var i = 0; i < lines.length; i++) {
            if (lines[i][0] === 'P') {
                this.planets.push(new Planet(lines[i]));
            } else if (lines[i][0] === 'F') {
                this.fleets.push(new Fleet(lines[i]));
            } else {
                console.log("Error: Unexpected line data = " + lines[i]);
            }
        }
    }

    draw() {
        for (var i = 0; i < this.planets.length; i++) {
            drawer.drawPlanet(this.planets[i]);
        }

        for (var j = 0; j < this.fleets.length; j++) {
            console.log(this.fleets[j]);
            drawer.drawFleet(
                this.fleets[j],
                this.planets[this.fleets[j].source],
                this.planets[this.fleets[j].destination]);
        }
    }
}

class Planet {
    constructor(line) {
        var elements = line.split(" ");
        if (elements[0] !== "P") {
            console.log("Error: Sent non-planet line into planet constructor");
            return;
        }
        this.x = parseFloat(elements[1]);
        this.y = parseFloat(elements[2]);
        this.owner = parseInt(elements[3]);
        this.ships = parseInt(elements[4]);
        this.growth = parseInt(elements[5]);
    }
}

class Fleet {
    constructor(line) {
        var elements = line.split(" ");
        if (elements[0] !== "F") {
            console.log("Error: Sent non-fleet line into planet constructor");
            return;
        }
        this.owner = parseInt(elements[1]);
        this.ships = parseInt(elements[2]);
        this.source = parseInt(elements[3]);
        this.destination = parseInt(elements[4]);
        this.totalTurns = parseInt(elements[5]);
        this.remainingTurns = parseInt(elements[6]);
    }
}