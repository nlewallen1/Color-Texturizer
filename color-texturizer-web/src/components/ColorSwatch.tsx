import React from 'react';
import type { ColorInfo } from '../types/color';

interface ColorSwatchProps {
    color: ColorInfo;
    isTextured: boolean;
}

export const ColorSwatch: React.FC<ColorSwatchProps> = ({ color, isTextured}) => {
    const tooltip = `${color.name}\nR:${color.r} G:${color.g} B:${color.b}`;

    return (
    <div
        title={tooltip}
        style={{
        width: '60px',
        height: '60px',
        margin: '15px',
        border: '1px solid #000000',
        boxSizing: 'border-box',
        backgroundColor: `rgb(${color.r}, ${color.g}, ${color.b})`,
        backgroundImage: isTextured && color.swatchBase64 ? `url(${color.swatchBase64})` : 'none',
        backgroundSize: 'cover',
        backgroundPosition: 'center',
        }}
    />
    );
};