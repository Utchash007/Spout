// Lightweight SignalR connection for live badge updates across all pages
(function () {
    if (typeof signalR === 'undefined') return;

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Error)
        .build();

    connection.on("ReceiveMessage", (msg) => {
        // Increment the message badge count in the sidebar
        const badge = document.querySelector('.nav-item[href*="Messages"] .item-count');
        if (badge) {
            const current = parseInt(badge.textContent) || 0;
            badge.textContent = current + 1;
            badge.style.display = 'flex';
        }
        
        // Dispatch custom event to invalidate messages cache
        window.dispatchEvent(new CustomEvent('newMessageReceived'));
    });

    connection.start().catch(err => console.error("Badge SignalR error:", err));
})();
