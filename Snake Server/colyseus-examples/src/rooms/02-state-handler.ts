import { Room, Client } from "colyseus";
import { Schema, type, MapSchema, ArraySchema } from "@colyseus/schema";
import { resolve } from "dns";

export class Vector2Float extends Schema {
    @type("uint32") id = 0;
    @type("number") x = Math.floor(Math.random() * 256) - 128;
    @type("number") z = Math.floor(Math.random() * 256) - 128;

}

export class Player extends Schema {
    @type("string") login = "";
    @type("number")
    x = Math.floor(Math.random() * 256) - 128;

    @type("number")
    z = Math.floor(Math.random() * 256) - 128;

    @type("uint8")
    d = 2;
    @type("uint8")
    skin = 0;
    @type("uint16")
    score = 0;
}

export class State extends Schema {
    @type({ map: Player }) players = new MapSchema<Player>();
    @type([Vector2Float]) apples = new ArraySchema<Vector2Float>();
    gameOverIDs = [];
    appleLastId = 0;

    createApple() {
        const apple = new Vector2Float();
        apple.id = this.appleLastId++;
        this.apples.push(apple)

    }



    collectApple(player: Player, data: any) {
        const apple = this.apples.find((value) => value.id === data.id);

        if (apple === undefined) return;

        apple.x = Math.floor(Math.random() * 256) - 128;
        apple.z = Math.floor(Math.random() * 256) - 128;

        player.score++;
        player.d = Math.floor(player.score / 5);
    }

    createPlayer(sessionId: string, skin: number, login) {
        const player = new Player();
        player.skin = skin;
        player.login = login;
        this.players.set(sessionId, player);
    }

    removePlayer(sessionId: string) {

        if (!this.players.has(sessionId)) return;

        this.players.delete(sessionId);
    }

    movePlayer(sessionId: string, movement: any) {
        this.players.get(sessionId).x = movement.x;
        this.players.get(sessionId).z = movement.z;
    }
    gameOver(data) {
        const detailsPositions = JSON.parse(data);
        const clientID = detailsPositions.id;

        const gameOverID = this.gameOverIDs.find((value) => value === clientID);

        if (gameOverID !== undefined) return;


        this.gameOverIDs.push(clientID);
        this.delayClearGameOverIDs(clientID);


        this.removePlayer(clientID);


        for (let i = 0; i < detailsPositions.ds.length; i++) {
            const apple = new Vector2Float();
            apple.id = this.appleLastId++;
            apple.x = detailsPositions.ds[0].x;
            apple.z = detailsPositions.ds[0].z;
            this.apples.push(apple)
        }
    }

    async delayClearGameOverIDs(clientID) {
        await new Promise(resolve => setTimeout(resolve, 10000))

        const index = this.gameOverIDs.findIndex((value) => value === clientID);
        if (index <= -1) return;

        this.gameOverIDs.splice(index, 1);
    }
}

export class StateHandlerRoom extends Room<State> {
    maxClients = 4;
    startAppleCount = 100;
    skins: number[] = [0];



    mixArray(arr) {
        var currentIndex = arr.length;
        var tmpValue, randomIndex;

        while (currentIndex !== 0) {
            randomIndex = Math.floor(Math.random() * currentIndex);
            currentIndex -= 1;
            tmpValue = arr[currentIndex];
            arr[currentIndex] = arr[randomIndex];
            arr[randomIndex] = tmpValue;
        }

    }

    onCreate(options) {

        const skinsCount = typeof options.skins === "number" ? options.skins : 1;

        this.skins = [];
        for (let i = 0; i < skinsCount; i++) {
            this.skins.push(i);
        }

        this.mixArray(this.skins);
        this.setState(new State());

        this.onMessage("gameOver", (client, data) => {

            this.state.gameOver(data);

        });
        this.onMessage("move", (client, data) => {
            this.state.movePlayer(client.sessionId, data);
        });
        this.onMessage("collect", (client, data) => {
            const player = this.state.players.get(client.sessionId);
            this.state.collectApple(player, data);
        });

        for (let i = 0; i < this.startAppleCount; i++) {
            this.state.createApple();
        }
    }

    onAuth(client, options, req) {
        return true;
    }

    onJoin(client: Client, data) {
        const skin = this.clients.length % 4;
        console.log("Player joined, skin =", skin);
        this.state.createPlayer(client.sessionId, skin, data.login);
    }

    onLeave(client) {
        this.state.removePlayer(client.sessionId);
    }

    onDispose() {
        console.log("Dispose StateHandlerRoom");
    }

}
