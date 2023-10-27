# Ninjatader 8 Indicator - NinjaSocket

NinjaSocket is an example WebSocket client for NinjaTrader 8.

## Indicator Install

- First, download [NinjaSocket.zip](NinjaSocket.zip) to your desktop, keep it in the compressed .zip file
- Next navigate to the 'Control Center' window and select  'Tools' from the menu, then 'Import', lastly select 'NinjaScript Add-on...'
- Then select the downloaded NinjaSocket.zip file
- NinjaTrader will then confirm if the import has been successful 

## Example
Below is a [Python example](python_example/main.py) that demonstrates a WebSocket server connecting to NinjaSocket.

```
git clone https://github.com/mattalford/ninja-socket.git
cd python_example
pip install -r requirements.txt
uvicorn main:app --reload
```
#### Send data to NinjaSocket
Go to http://localhost:8000/client1/call/send
```
NinjaScript Output:
Data: Sent data - Hello Word!
```

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details
