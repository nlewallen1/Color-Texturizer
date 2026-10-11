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
            <HeaderControls
                onUpload={handleUpload}
                onApplyTexture={handleApplyTexture}
                isProcessing={isProcessing}
                hasFile={!!selectedFile}
            />
            {/* image display component */}
            <ImageDisplay imageUrl={displayedImageUrl} />
            {/* color panel component */}
            <ColorPanel colors={colors} isTextured={isTextured} />
        </div>
    );
}

export default App;