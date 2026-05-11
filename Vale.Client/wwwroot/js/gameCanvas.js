window.valeCanvas = (() => {
    const tileWidth = 26;
    const tileHeight = 14;
    const originX = 640;
    const originY = 70;

    function initialize(canvas) {
        if (!canvas) return;
        const ctx = canvas.getContext("2d");
        ctx.imageSmoothingEnabled = false;
    }

    function isoToScreen(x, y) {
        return {
            x: (x - y) * (tileWidth / 2) + originX,
            y: (x + y) * (tileHeight / 2) + originY
        };
    }

    function screenToWorld(screenX, screenY) {
        const a = (screenX - originX) / (tileWidth / 2);
        const b = (screenY - originY) / (tileHeight / 2);

        return {
            x: (a + b) / 2,
            y: (b - a) / 2
        };
    }

    function drawDiamond(ctx, x, y, fillStyle, strokeStyle) {
        ctx.beginPath();
        ctx.moveTo(x, y);
        ctx.lineTo(x + tileWidth / 2, y + tileHeight / 2);
        ctx.lineTo(x, y + tileHeight);
        ctx.lineTo(x - tileWidth / 2, y + tileHeight / 2);
        ctx.closePath();

        ctx.fillStyle = fillStyle;
        ctx.fill();
        ctx.strokeStyle = strokeStyle;
        ctx.stroke();
    }

    function render(canvas, payload) {
        if (!canvas || !payload) return;

        const ctx = canvas.getContext("2d");
        ctx.fillStyle = "#111312";
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        for (let y = 0; y < payload.chunkSize; y++) {
            for (let x = 0; x < payload.chunkSize; x++) {
                const p = isoToScreen(x, y);
                const shade = (x + y) % 2 === 0 ? "#2a2f26" : "#262b22";
                drawDiamond(ctx, p.x, p.y, shade, "#181b17");
            }
        }

        for (const structure of payload.structures) {
            const p = isoToScreen(structure.tileX, structure.tileY);
            let color = "#555";
            if (structure.structureType === 1) color = "#7a7f7f";
            if (structure.structureType === 2) color = "#3c342e";
            if (structure.structureType === 3) color = "#57524d";
            drawDiamond(ctx, p.x, p.y, color, "#0f0f0f");
        }

        for (const player of payload.players) {
            const p = isoToScreen(player.x, player.y);
            ctx.beginPath();
            ctx.arc(p.x, p.y + 7, 4.5, 0, Math.PI * 2);
            ctx.fillStyle = player.playerId === payload.currentPlayerId ? "#f1873f" : "#8d8fb3";
            ctx.fill();
            ctx.fillStyle = "#dfd8cf";
            ctx.font = "11px Segoe UI";
            ctx.fillText(player.displayName, p.x + 7, p.y + 2);
        }
    }

    return {
        initialize,
        render,
        screenToWorld
    };
})();
