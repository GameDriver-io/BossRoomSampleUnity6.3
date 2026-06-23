
extern "C" {
	int WebSocketConnect(int instanceId){return 0;}
	int WebSocketClose(int instanceId, int code, char * reason) {return 0;}
	int WebSocketSend(int instanceId, void * ptr, int dataLength) { return 0; }
	int WebSocketGetState(int instanceId) { return 0; }
	int WebSocketAllocate(char * url) { return 0; }
	void WebSocketFree(int instanceId) { return; }
	void WebSocketSetOnOpen(void * ptr) { return; }
	void WebSocketSetOnMessage(void * ptr) { return; }
	void WebSocketSetOnError(void * ptr) { return; }
	void WebSocketSetOnClose(void * ptr) { return; }
}
