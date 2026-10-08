import { useState } from 'react';
import type { ColorInfo } from '../types/color';
import { extractColors, processImage } from '../services/api';

export function useColorTexturizer() {
    // set state variables
    const [selectedFile, setSelectedFile] = useState<File | null>(null);
    const [displayedImageUrl, setDisplayedImageUrl] = useState<string | null>(null);
    const [colors, setColors] = useState<ColorInfo[]>([]);
    const [isTextured, setIsTextured] = useState<boolean>(false);
    const [isProcessing, setIsProcessing] = useState<boolean>(false);

    // handle image upload
    const handleUpload = async (file: File) => {
        // get image and reset
        setSelectedFile(file);
        setDisplayedImageUrl(URL.createObjectURL(file));
        setColors([]);
        setIsTextured(false);
        setIsProcessing(true);

        // extract
        try {
            const extractedColors = await extractColors(file);
            setColors(extractedColors);
        } catch (err) {
            alert(err instanceof Error ? err.message : 'Failed to upload image.');
        } finally {
            setIsProcessing(false);
        }
    };

    // handle applying textures
    const handleApplyTexture = async () => {
        // 
        if (!selectedFile) {
            alert('Please upload an image first.');
            return
        }

        setIsProcessing(true);

        // get textured image
        try {
            const texturedBlobUrl = await processImage(selectedFile);
            setDisplayedImageUrl(texturedBlobUrl);
            setIsTextured(true);
        } catch (err) {
            alert(err instanceof Error ? err.message : 'Failed to apply textures.');
        } finally {
            setIsProcessing(false);
        }
    };

    return {
        selectedFile,
        displayedImageUrl,
        colors,
        isTextured,
        isProcessing,
        handleUpload,
        handleApplyTexture,
    };
}