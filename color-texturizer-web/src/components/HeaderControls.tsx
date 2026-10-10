import React from 'react';

interface HeaderControlProps {
    onUpload: (file: File) => void;
    onApplyTexture: () => void;
    isProcessing: boolean;
    hasFile: boolean;
}

// component for header controls
export const HeaderControls: React.FC<HeaderControlsProps> = ({
    onUpload,
    onApplyTexture,
    isProcessing,
    hasFile,
}) => {
    // handle a new file
    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file) onUpload(file);
    }

    return (
        // set position in parent
        <div style = {{
            gridRow: 1,
            gridColumn: '1 / span 2',
            display: 'flex',
            alignItems: 'center',
            padding: '10px'
        }}>
            {/* add a label accepting an upload, make it apper as a button */}
            <label style = {buttonStyle}>
                Upload
                <input
                type="file"
                accept=".png,.jpg,.jpeg"
                onChange={handleFileChange}
                style={{ display: 'none' }}
                />
            </label>

            {/* add texture button */}
            <button
            onClick = {onApplyTexture}
            disabled={isProcessing || !hasFile}
            style = {{
                ...buttonStyle,
                cursor: isProcessing || !hasFile ? 'default' : 'pointer',
                opacity: isProcessing || !hasFile ? 0.6 : 1,
            }}
            >

                {isProcessing ? 'Processing...' : 'Add texture'}
            </button>
        </div>
    )
};

// material design button style
const buttonStyle: React.CSSProperties = {
  width: '130px',
  height: '36px',
  margin: '10px',
  backgroundColor: '#424242',
  color: '#ffffff',
  border: '1px solid #616161',
  borderRadius: '2px',
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  cursor: 'pointer',
  fontSize: '14px',
  boxShadow: '0px 2px 4px rgba(0,0,0,0.3)',
};