import React from 'react';
import type { ColorInfo } from '../types/color';
import { ColorSwatch } from './ColorSwatch'

interface ColorPanelProps {
    colors: ColorInfo[];
    isTextured: boolean;
}

// new component using ColorPanelProps interface
export const ColorPanel: React.FC<ColorPanelProps> = ({colors, isTextured}) => {
    return (
        <div style = {{
            margin: '30px',
            display: 'flex',
            flexWrap: 'wrap',
            alignContent: 'flex-start',
            overflowY: 'auto'
        }}>
            {/* return array of color swatches */}
            {colors.map((color, idx) => (
                <ColorSwatch key={idx} color={color} isTextured={isTextured} />
            ))}
        </div>
    );
} 