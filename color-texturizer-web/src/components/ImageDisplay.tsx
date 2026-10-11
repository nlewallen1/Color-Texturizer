import React from 'react';

interface ImageDisplayProps {
  imageUrl: string | null;
}

export const ImageDisplay: React.FC<ImageDisplayProps> = ({ imageUrl }) => {
  return (
    <div style={{
      gridRow: 2,
      gridColumn: 1,
      margin: '20px 0 0 20px',
      overflow: 'auto',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      backgroundColor: '#212121',
      border: '1px solid #424242'
    }}>
      <div style={{ position: 'relative', display: 'inline-block' }}>
        {/* display image if it exists */}
        {imageUrl && (
          <img
            src={imageUrl}
            alt="Loaded Content"
            style={{ display: 'block', maxWidth: '100%', height: 'auto' }}
          />
        )}
      </div>
    </div>
  );
};