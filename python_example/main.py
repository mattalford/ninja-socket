from fastapi import FastAPI, WebSocket, HTTPException  
  
app = FastAPI()  
  
class ConnectionManager:    
    def __init__(self):    
        self.active_connections: dict = {}    
    
    async def connect(self, client_id: str, websocket: WebSocket):    
        await websocket.accept()    
        self.active_connections[client_id] = websocket    
    
    async def disconnect(self, client_id: str):    
        if client_id in self.active_connections:    
            self.active_connections.pop(client_id)    
    
    async def send_command(self, client_id: str, command: str):    
        if client_id in self.active_connections:    
            await self.active_connections[client_id].send_text(command)    
        else:    
            raise Exception("No active connection")    
  
    def count_active_connections(self):  
        return len(self.active_connections)  
 
  
manager = ConnectionManager()  
  
@app.websocket("/ws/{client_id}")  
async def websocket_endpoint(websocket: WebSocket, client_id: str):  
    await manager.connect(client_id, websocket)  
    try:  
        while True:  
            data = await websocket.receive_text()  
            # Handle received data
            print(data) 
    except Exception as e:  
        print(e)  
    finally:  
        await manager.disconnect(client_id)  
  
@app.get("/{client_id}/call/send")  
async def call_send(client_id: str):  
    try:  
        # Send data
        textString = "Text to send"
        await manager.send_command(client_id, textString)
        return {"message": "Command sent"}  
    except Exception as e:  
        raise HTTPException(status_code=500, detail=str(e))  

@app.get("/")
async def root():
    return {"message": "Server Running"}

@app.get("/connections")  
async def connections():  
    return {"active_connections": manager.count_active_connections()} 