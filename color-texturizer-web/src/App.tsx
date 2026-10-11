import React from 'react';
import { useColorTexturizer } from './hooks/useColorTexturizer';
import { HeaderControls } from './components/HeaderControls';
import { ImageDisplay } from './components/ImageDisplay';
import { ColorPanel } from './components/ColorPanel';

export function App() {
    // call useColorTexturizer hook and retrieve data
    const {
        selectedFile,
        displayedImageUrl,
        colors,
        isTextured,
        isProcessing,
        handleUpload,
        handleApplyTexture,
    } = useColorTexturizer();

    return (
        // set a grid
        <div style={{
            display: 'grid',
            gridTemplateRows: 'auto 1fr auto',
            gridTemplateColumns: '2fr 1fr',
            height: '100vh',
            width: '100vw',
            backgroundColor: '#303030'
        }}>
            {/* header controls component */}
            <div style={{ gridRow: 1, gridColumn: '1 / span 2' }}>
                <HeaderControls
                    onUpload={handleUpload}
                    onApplyTexture={handleApplyTexture}
                    isProcessing={isProcessing}
                    hasFile={!!selectedFile}
                />
            </div>
            {/* image display component */}
            <div style={{ gridRow: 2, gridColumn: 1 }}>
                <ImageDisplay imageUrl={displayedImageUrl} />
            </div>

            {/* color panel component */}
            <div style={{ gridRow: 2, gridColumn: 2 }}>
                <ColorPanel colors={colors} isTextured={isTextured} />
            </div>
        </div>
    );
}

export default App;